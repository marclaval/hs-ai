#!/usr/bin/env bash

set -e -o pipefail

cd ./external_projects/SabberStone
git pull
cd ../..

cd ./external_projects/Hearthstone-Deck-Tracker
git pull
cd ../..
