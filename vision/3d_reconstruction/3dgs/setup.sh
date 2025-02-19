#!/bin/bash

### To train the 3D Gaussian:
# PLEASE MAKE SURE YOU DELETE  OUTPUT FOLDER UNDER SuGaR BEFORE RUNNING THIS!!!!
# Follow this first: https://github.com/graphdeco-inria/gaussian-splatting/issues/272

apptainer exec --nv --fakeroot --env NVDIFRAST_CUDA_RASTERIZER=1 --overlay overlay.img --bind workspaceperfcam:/workspace SuGaR.sif bash -c "cd /root/SuGaR && pip install \"git+https://github.com/michelgokan/SuGaR.git#egg=diff_gaussian_rasterization&subdirectory=gaussian_splatting/submodules/diff-gaussian-rasterization\" && NVDIFRAST_CUDA_RASTERIZER=1 python train_full_pipeline.py -s /workspace -r dn_consistency --high_poly True --export_obj True"

