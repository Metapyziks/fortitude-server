#!/bin/bash

OLDDIR="$(pwd)"
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

echo "Entering $DIR"
cd $DIR

MONO="$(pidof mono)"
if [ $MONO ]
then
	echo "Stopping active server with pid ${MONO}"
	kill $MONO
else
	echo "Server not currently running, so will not be restarted"
fi

echo "Pulling new changes"
git pull origin live
echo "Making"
make release

if [ $MONO ]
then
	echo "Starting server"
	bash init.sh >init.log 2>&1
fi

echo "Leaving $DIR"
cd $OLDDIR
