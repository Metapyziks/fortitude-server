#!/bin/sh

monopath=/usr/local/bin/mono

projdir=/root/fortitude-server/
binpath=${projdir}bin/release/TestServer.exe
logpath=${projdir}console.log

$monopath $binpath >$logpath 2>&1 &
