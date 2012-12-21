#!/bin/bash

OLDDIR=pwd
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

echo "Entering $DIR"
cd $DIR

MONO=pidof mono
if [ -n $MONO ] ; then
	echo "Stopping active server with pid ${MONO}"
	kill $MONO
fi

echo "Pulling new changes"
git pull origin live
echo "Making"
make release
echo "Starting server"
sh init.sh

echo "Entering $OLDDIR"
cd $OLDDIR
