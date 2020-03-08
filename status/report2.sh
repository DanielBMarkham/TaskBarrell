#! /bin/bash

REPORTFILE=report-`date +%F`.txt
SOURCEDIR=${1:-$PWD}
TARGETDIR=${2:-$PWD}

rm "$TARGETDIR/$REPORTFILE" || true
sort $SOURCEDIR/*.txt | for n in *.txt; do printf '%s\n' "$n"; printf '%s\n' "-------"; cat "$n"; printf '\n'; done > report.tmp && mv report.tmp "$TARGETDIR/$REPORTFILE"

