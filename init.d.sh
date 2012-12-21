#!/bin/bash
# /etc/init.d/fortitude
#

case "$1" in
  start)
    echo "Starting fortitude..."
    bash /root/fortitude/init.sh
    ;;
  stop)
    echo "Stopping fortitude..."
    bash /root/fortitude/stop.sh
    ;;
  update)
    echo "Updating fortitude..."
    bash /root/fortitude/update.sh
  *)
    echo "Usage: /etc/init.d/fortitude {start|stop|update}"
    exit 1
    ;;
esac

exit 0

