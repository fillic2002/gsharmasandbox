set PATH=%PATH%;C:\Program Files\7-Zip\
REM curl https://artifacts.elastic.co/downloads/elasticsearch/elasticsearch-7.5.0-windows-x86_64.zip -o C:\Users\fillic\Documents\Professional\binary\e.zip
REM #curl https://artifacts.elastic.co/downloads/logstash/logstash-7.5.0.zip -o C:\Users\fillic\Documents\Professional\binary\l.zip
REM #curl https://artifacts.elastic.co/downloads/kibana/kibana-7.5.0-windows-x86_64.zip -o C:\Users\fillic\Documents\Professional\binary\k.zip
REM curl https://downloads.apache.org/kafka/2.6.0/kafka_2.13-2.6.0.tgz -o C:\Users\fillic\Documents\Professional\binary\kfk.tgz
curl http://packages.treasuredata.com.s3.amazonaws.com/3/windows/td-agent-3.1.1-0-x64.msi -o C:\Users\fillic\Documents\Professional\binary\f.zip

cd C:\Users\fillic\Documents\Professional\binary
REM 7z x e.zip
REM 7z x k.zip
REM 7z x l.zip
REM 7z x kfk.tgz
REM 7z x kfk.tar
REM del e.zip
REM del k.zip
REM del l.zip
REM del kfk.tgz
REM del kfk.tar
