curl https://www.7-zip.org/a/7z1900-x64.exe -o C:\software
curl https://javadl.oracle.com/webapps/download/AutoDL?xd_co_f=YWU3ZDZiNTQtZDA5Yy00NWY0LWJkNzYtMWY0ODliZmUwZWI0&BundleId=242959_a4634525489241b9a9e1aa73d9e118e6 -o C:\software
set PATH=%PATH%;C:\Program Files\7-Zip\
curl https://artifacts.elastic.co/downloads/elasticsearch/elasticsearch-7.5.0-windows-x86_64.zip -o C:\software\e.zip
curl https://artifacts.elastic.co/downloads/logstash/logstash-7.5.0.zip -o C:\software\l.zip
curl https://artifacts.elastic.co/downloads/kibana/kibana-7.5.0-windows-x86_64.zip -o C:\software\k.zip
curl https://downloads.apache.org/kafka/2.6.0/kafka_2.13-2.6.0.tgz -o C:\software\kfk.tgz
curl http://packages.treasuredata.com.s3.amazonaws.com/3/windows/td-agent-3.1.1-0-x64.msi -o C:\software\f.zip
curl https://curl.haxx.se/windows/

cd C:\software
 7z x e.zip
 7z x k.zip
 7z x l.zip
 7z x kfk.tgz
 7z x kfk.tar
 del e.zip
 del k.zip
 del l.zip
 del kfk.tgz
 del kfk.tar
