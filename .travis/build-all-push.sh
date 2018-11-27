#!/bin/bash

scriptPath=$(dirname "$(readlink -f "$0")")

echo "${DOCKER_PASSWORD}" | docker login -u "${DOCKER_USERNAME}" --password-stdin

${scriptPath}/../packages/ove-asset-manager/build.sh -v unstable --push && \
${scriptPath}/../packages/ove-service-imagetiles/build.sh -v unstable --push