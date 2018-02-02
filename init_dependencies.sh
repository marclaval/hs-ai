#!/usr/bin/env bash

set -e -o pipefail

mkdir -p external_projects

git clone https://github.com/marclaval/SabberStone.git ./external_projects/SabberStone
cd external_projects/SabberStone
git checkout RandomController
cd ../..

git clone https://github.com/HearthSim/Hearthstone-Deck-Tracker.git ./external_projects/Hearthstone-Deck-Tracker
cd external_projects/Hearthstone-Deck-Tracker
cmd "/C bootstrap.bat"
cd ../..
