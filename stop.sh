#!/bin/sh

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
	echo "Server not running"
fi

echo "Leaving $DIR"
cd $OLDDIR
