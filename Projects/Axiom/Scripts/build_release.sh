#!/bin/sh

mono ../../../BuildSupport/nAnt/bin/NAnt.exe -buildfile:../Axiom.build release build.axiom -l:../Axiom.build.release.log
