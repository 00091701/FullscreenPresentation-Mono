#!/bin/bash

pwd=`pwd`

TMPDIR=`mktemp -d /tmp/ANDROID-Market.XXXXXXXX` || exit 1
APK="de.dhoffmann.mono.fullscreenpresentation.droid.apk"

cp ../../../../../Android-Keystore/AndNaviki ${TMPDIR}/DH-Google.keystore
cp ../../FP.Droid/bin/Release/${APK} ${TMPDIR}/${APK}

cd $TMPDIR

FIRSTLS=`ls -l ${APK}`

jarsigner -verbose -keystore DH-Google.keystore ${APK} AndNaviki
/Users/david/Library/Developer/Xamarin/android-sdk-mac_x86/tools/zipalign -v 4 ${APK} MARKET_${APK}

cp MARKET_${APK} ${pwd}/MARKET_${APK}

