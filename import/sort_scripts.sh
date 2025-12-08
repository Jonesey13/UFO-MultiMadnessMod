#!/usr/bin/env sh

cd "complete_original_scripts"

if [ ! -f 'gml_GlobalScript_scrLoadInternalText.gml' ]; then
    echo >&2 "Missing text script -- have you extracted everything to 'complete_original_scripts'?"
    exit 1
fi

grep -E 'game_name_([1-9]|[1-5][0-9]) =' gml_GlobalScript_scrLoadInternalText.gml | sort | uniq | cut -d_ -f4 | sed 's/[^0-9A-Z ]//g' | sort -n | while IFS= read line; do
    set -- $line
    num="$(printf '%02d' "$1")"
    shift
    name="$(echo "$@" | tr '[:upper:]' '[:lower:]' | tr ' ' '_')"
    echo $num: $name
    mkdir -p $name
    mv -v gml_Object_o${num}_*.gml gml_GlobalScript_scr${num}_*.gml gml_Room*_rm${num}_*.gml $name/
done