#!/usr/bin/env bash

set -e -o pipefail

mkdir -p external_projects

git clone https://github.com/marclaval/SabberStone.git ./external_projects/SabberStone
cd external_projects/SabberStone
git checkout RandomController
cd ../..

git clone https://github.com/marclaval/Hearthstone-Deck-Tracker.git ./external_projects/Hearthstone-Deck-Tracker
cd external_projects/Hearthstone-Deck-Tracker
git checkout moreEventHandlers
cmd "/C bootstrap.bat"
cd ../..
