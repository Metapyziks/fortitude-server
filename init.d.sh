#!/bin/bash
# /etc/init.d/fortitude
#

ftdir=/root/fortitude-server/

case "$1" in
  start)
    echo "Starting fortitude..."
    bash ${ftdir}init.sh >${ftdir}init.log 2>&1
    ;;
  stop)
    echo "Stopping fortitude..."
    bash ${ftdir}stop.sh >${ftdir}stop.log 2>&1
    ;;
  update)
    echo "Updating fortitude..."
    bash ${ftdir}update.sh >${ftdir}update.log 2>&1
    ;;
  *)
    echo "Usage: /etc/init.d/fortitude {start|stop|update}"
    exit 1
    ;;
esac

exit 0

