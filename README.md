```
fhash2 for CNT4513 UHA1188

Generates MD5 and/or SHA1 hashes based on a given file/dir path,
can save the results to a .csv file

USAGE:
    fhash FILE_PATH | DIR_PATH [[-md5] | [-sha1]] [-raw] [-pause] [-quiet]
                               [-verbose] [[-csv FILE_PATH] | [-sort]] [-help]

OPTIONS:
    -md5              - Calculate MD5 hash
    -sha1             - Calculate SHA1 hash
    -raw              - Exclusively return Hash Value
    -pause            - Wait for keystroke before exiting
    -quiet            - Display no messages
    -verbose          - Display all messages
    -csv FILE_PATH    - Save hashes to CSV
    -sort             - Return CSV hashes sorted by filepath a-Z
    -help             - Display this message again.

EXAMPLES:
    > fhash test.txt
    > fhash test.txt -md5
    > fhash test.txt -sha1
    > fhash C:/test -sha1 -md5
    > fhash C:/test -sha1 -md5 -csv C:/output.csv

ARGUMENTS:
    -md5 /md5
    -sha1 /sha1
    -raw /raw -r /r
    -pause /pause -p /p
    -quiet /quiet -q /q
    -verbose /verbose -v /v
    -csv /csv
    -sort /sort -s /s
    -help /help -h /h
