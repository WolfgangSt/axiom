using System;
using Axiom.Core;
using Axiom.ParticleSystems;
using Axiom.Scripting;

namespace ParticleFX
{
	/// <summary>
	/// 	Summary description for PointEmitterFactory.
	/// </summary>
	public class PointEmitterFactory : ParticleEmitterFactory {
	
		#region Methods

		public override ParticleEmitter Create() {
			PointEmitter emitter = new PointEmitter();
			emitterList.Add(emitter);
			return emitter;
		}
		
		#endregion
		
		#region Properties

		public override string Name	{
			get {
				return "Point";
			}
		}
	
		#endregion

	}
}