using System;
using System.Collections.Generic;
using Axiom.Collections;
using System.Reflection;

using Axiom.Core;

namespace Axiom.Scripting
{
	public sealed class ScriptableProperties
	{
		private IScriptableObject _owner;
		public ScriptableProperties( IScriptableObject owner )
		{
			_owner = owner;
		}

		public string this[ string property ]
		{
			get
			{
				return _owner[ property ];
			}
			set
			{
				_owner[ property ] = value;
			}
		}
	}

	/// <summary>
	/// An interface to provide access to object properties through strings
	/// </summary>
	public interface IScriptableObject
	{
		/// <summary>
		/// The property collection available
		/// </summary>
		ScriptableProperties Properties
		{
			get;
		}

		/// <summary>
		/// Set multiple properties at once using a <see cref="NameValuePairList"/>
		/// </summary>
		/// <param name="parameters">the collection of parameters</param>
		void SetParameters( NameValuePairList parameters );

		/// <summary>
		/// provides access to each property
		/// </summary>
		/// <param name="index">name of the property</param>
		/// <returns>the property value</returns>
		string this[ string index ]
		{
			get;
			set;
		}
	}

	[AttributeUsage( AttributeTargets.Class, AllowMultiple = true )]
	public sealed class ScriptablePropertyAttribute : Attribute
	{
		public readonly string ScriptPropertyName;
		public ScriptablePropertyAttribute( string scriptPropertyName )
		{
			ScriptPropertyName = scriptPropertyName;
		}

		public ScriptablePropertyAttribute( string scriptPropertyName, string description, Type owner )
		{
			ScriptPropertyName = scriptPropertyName;
		}
	}

	public abstract class ScriptableObject : DisposableObject, IScriptableObject
	{
		private Dictionary<String, IPropertyCommand> _classParameters;
		/// <summary>
		///
		/// </summary>
		public ICollection<IPropertyCommand> Commands
		{
			get
			{
				return _classParameters.Values;
			}
		}

		/// <summary>
		///
		/// </summary>
		protected ScriptableObject()
		{
			_classParameters = _getTypePropertyMap( this.GetType() );
			_properties = new ScriptableProperties( this );
		}

		#region Static Implementation

		private static Dictionary<Type, Dictionary<String, IPropertyCommand>> _propertyMaps = new Dictionary<Type, Dictionary<string, IPropertyCommand>>();

		private static Dictionary<String, IPropertyCommand> _getTypePropertyMap( Type type )
		{
			Dictionary<String, IPropertyCommand> list;
			if ( !_propertyMaps.TryGetValue( type, out list ) )
			{
				list = new Dictionary<string, IPropertyCommand>();
				_propertyMaps.Add( type, list );
				// Use reflection to load the mapping between script name and IPropertyCommand
				_initializeTypeProperties( type, list );
			}
			return list;
		}

		private static void _initializeTypeProperties( Type type, Dictionary<string, IPropertyCommand> list )
		{
			foreach ( Type nestType in type.GetNestedTypes( BindingFlags.NonPublic | BindingFlags.Public ) )
			{
#if !(XBOX || XBOX360)
				if ( nestType.FindInterfaces( delegate( Type typeObj, Object criteriaObj )
												{
													if ( typeObj.ToString() == criteriaObj.ToString() )
														return true;
													else
														return false;
												}
											, typeof( IPropertyCommand ).FullName ).Length != 0 )
				{
					foreach ( ScriptablePropertyAttribute attr in nestType.GetCustomAttributes( typeof( ScriptablePropertyAttribute ), true ) )
					{
						IPropertyCommand propertyCommand = (IPropertyCommand)Activator.CreateInstance( nestType );
						list.Add( attr.ScriptPropertyName, propertyCommand );
					}
				}
#else
				foreach ( Type iface in nestType.GetInterfaces() )
				{
					if ( iface.FullName == typeof(IPropertyCommand).FullName )
					{
						foreach (ScriptablePropertyAttribute attr in nestType.GetCustomAttributes(typeof(ScriptablePropertyAttribute), true))
						{
							IPropertyCommand propertyCommand = (IPropertyCommand)Activator.CreateInstance(nestType);
							list.Add(attr.ScriptPropertyName, propertyCommand);
						}
					}
				}
#endif
			}
			// Load the IPropertyCommands from the parent Type
			if ( type.BaseType != typeof( System.Object ) )
			{
				foreach ( KeyValuePair<string, IPropertyCommand> item in _getTypePropertyMap( type.BaseType ) )
				{
					list.Add( item.Key, item.Value );
				}
				type = type.BaseType;
			}
		}

		#endregion Static Implementation

		#region Implementation of IScriptableObject

		private ScriptableProperties _properties;
		/// <summary>
		/// a list of properties accessible through though a string interface
		/// </summary>
		public ScriptableProperties Properties
		{
			get
			{
				return _properties;
			}
		}

		/// <summary>
		/// Set multiple properties using a <see cref="NameValuePairList"/>
		/// </summary>
		/// <param name="parameters">the list of properties to set</param>
		public void SetParameters( NameValuePairList parameters )
		{
			foreach ( KeyValuePair<String, String> item in parameters )
			{
				this.Properties[ item.Key ] = item.Value;
			}
		}

		// This is using explicit interface implementation to hide the inplementation from the public api
		// access to this indexer is provided through the Properties property
		string IScriptableObject.this[ string property ]
		{
			get
			{
				IPropertyCommand command;

				if ( _classParameters.TryGetValue( property, out command ) )
				{
					return command.Get( this );
				}
				else
				{
					LogManager.Instance.Write( "{0}: Unrecognized parameter '{1}'", this.GetType().Name, property );
				}
				return null;
			}
			set
			{
				IPropertyCommand command;

				if ( _classParameters.TryGetValue( property, out command ) )
				{
					command.Set( this, value );
				}
				else
				{
					LogManager.Instance.Write( "{0}: Unrecognized parameter '{1}'", this.GetType().Name, property );
				}
			}
		}

		#endregion Implementation of IScriptableObject
	}
}