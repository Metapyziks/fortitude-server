#!/bin/bash

OLDDIR=pwd
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

echo "Entering $DIR"
cd $DIR

git pull origin master
make release
sh init.sh

echo "Entering $OLDDIR"
cd $OLDDIR
