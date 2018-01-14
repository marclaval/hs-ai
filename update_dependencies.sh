#!/usr/bin/env bash

set -e -o pipefail

cd ./external_projects/SabberStone
git pull
cd ../..

cd ./external_projects/HearthDb
git pull
cd ../..
