#! /usr/bin/env bash
rm -R lib
wget https://github.com/UnderminersTeam/UndertaleModTool/releases/download/0.8.0.0/UTMT_CLI_v0.8.0.0-Ubuntu.zip -O undertalemodlib.zip
unzip -d lib undertalemodlib.zip
rm undertalemodlib.zip
cp -R ./lib/GameSpecificData /root/.dotnet/tools/.store/dotnet-script/1.6.0/dotnet-script/1.6.0/tools/net9.0/any