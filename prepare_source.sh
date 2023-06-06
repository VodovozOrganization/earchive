#!/bin/bash
echo "Что делаем?"
echo "1) git pull"
echo "2) nuget restore"
echo "3) cleanup packages directories"
echo "4) build Earchive.sln (Debug x86)"
echo "5) remove obj & bin folders"
echo "Можно вызывать вместе, например git+nuget=12"
read case;

case $case in
    *5*)
find . -type d -regex '.*\(bin\|obj\)' -exec rm -rv {} + 
;;&
    *3*)
rm -v -f -R ./packages/*
;;&
    *1*)
git pull --autostash --recurse-submodules -j8
;;&
    *2*)
nuget restore ./earchive.sln;
;;&
    *4*)
msbuild /p:Configuration=Debug /p:Platform=x86 ./earchive.sln -maxcpucount:4
;;&
esac

read -p "Press enter to exit"
