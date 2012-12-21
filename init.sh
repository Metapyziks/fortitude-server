#!/bin/sh

monopath=/usr/local/bin/mono

projdir=/root/fortitude-server/
binpath=${projdir}bin/release/TestServer.exe
logpath=${projdir}console.log

OLDDIR="$(pwd)"
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

echo "Entering $DIR"
cd $DIR

MONO="$(pidof mono)"
if [ $MONO ]
then
	echo "Stopping active server with pid ${MONO}"
	kill $MONO
fi

echo "Starting server"
nohup $monopath $binpath >$logpath 2>&1 &

echo "Entering $OLDDIR"
cd $OLDDIR
