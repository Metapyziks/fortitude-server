#!/bin/sh

monopath=/usr/local/bin/mono

OLDDIR="$(pwd)"
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

echo "Entering $DIR"
cd $DIR

binpath=bin/release/TestServer.exe
logpath=console.log

MONO="$(pidof mono)"
if [ $MONO ]
then
	echo "Stopping active server with pid ${MONO}"
	kill $MONO
fi

echo "Starting server"
nohup $monopath $binpath >$logpath 2>&1 &

echo "Leaving $DIR"
cd $OLDDIR
