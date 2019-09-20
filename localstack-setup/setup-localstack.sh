#!/bin/sh

set -e

until aws --region eu-west-1 --endpoint-url=http://localstack:4576 sqs list-queues; do
  >&2 echo "Localstack SQS is unavailable - sleeping"
  sleep 1
done

>&2 echo "Localstack SQS is up - executing command"
aws --region eu-west-1 --endpoint-url=http://localstack:4576 sqs create-queue --queue-name Samples-FullDuplex-Client
aws --region eu-west-1 --endpoint-url=http://localstack:4576 sqs create-queue --queue-name Samples-FullDuplex-Server
aws --region eu-west-1 --endpoint-url=http://localstack:4576 sqs create-queue --queue-name error
aws --region eu-west-1 --endpoint-url=http://localstack:4572 s3 mb s3://Samples-FullDuplex-Server-bucketname
aws --region eu-west-1 --endpoint-url=http://localstack:4572 s3 mb s3://Samples-FullDuplex-Server/bucketname
aws --region eu-west-1 --endpoint-url=http://localstack:4572 s3 mb s3://Samples-FullDuplex-Client-bucketname
aws --region eu-west-1 --endpoint-url=http://localstack:4572 s3 mb s3://Samples-FullDuplex-Client/bucketname