FROM xueshanf/awscli

COPY setup-localstack.sh .
RUN dos2unix setup-localstack.sh
RUN chmod +x setup-localstack.sh

CMD ["./setup-localstack.sh"]