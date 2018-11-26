#!/bin/bash

scriptPath=$(dirname "$(readlink -f "$0")")

${scriptPath}/../packages/ove-asset-manager/build.sh -v unstable && \
${scriptPath}/../packages/ove-service-imagetiles/build.sh -v unstable 