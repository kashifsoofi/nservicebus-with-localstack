#!/bin/sh

set -e

until aws --region eu-west-1 --endpoint-url=http://localstack:4566 sqs list-queues; do
  >&2 echo "Localstack SQS is unavailable - sleeping"
  sleep 1
done

>&2 echo "Localstack SQS is up - executing command"
aws --region eu-west-1 --endpoint-url=http://localstack:4566 sqs create-queue --queue-name Samples-FullDuplex-Client
aws --region eu-west-1 --endpoint-url=http://localstack:4566 sqs create-queue --queue-name Samples-FullDuplex-Server
aws --region eu-west-1 --endpoint-url=http://localstack:4566 sqs create-queue --queue-name error
aws --region eu-west-1 --endpoint-url=http://localstack:4566 s3api create-bucket --bucket bucketname
aws --endpoint-url=http://localstack:4566 s3api put-bucket-acl --bucket bucketname --acl public-read