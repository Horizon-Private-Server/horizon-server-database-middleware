#!/bin/bash
set -e

docker container kill horizon-middleware
bash run.sh
