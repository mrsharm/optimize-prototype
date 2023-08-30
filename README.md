# Optimization Prototype 

This repository consists of a prototype making use of Bayesian Optimization and crank on GC Data. 

More specifically, we vary the LOH Threshold and run a GCPerfSim scenario that predominately allocates on the LOH. The Bayesian Optimization Algorithm then tries to find the optimum % Pause Time in GC by intelligently varying the LOH threshold.

## Prerequisites

1. Python
2. Pipenv
3. .NET 7
