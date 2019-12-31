for n in *.txt; do printf '%s\n' "$n"; cat "$n"; printf '\n'; done
