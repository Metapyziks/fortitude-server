#!/bin/sh

monopath=/usr/local/bin/mono

projdir=/root/fortitude-server/
binpath=${projdir}bin/release/TestServer.exe
logpath=${projdir}console.log

/usr/local/bin/mono $binpath >$logpath 2>&1 &
