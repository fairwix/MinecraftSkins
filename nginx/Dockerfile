FROM nginx:alpine

RUN apk add --no-cache openssl
COPY docker-entrypoint.d/20-generate-local-cert.sh /docker-entrypoint.d/20-generate-local-cert.sh
RUN chmod 0755 /docker-entrypoint.d/20-generate-local-cert.sh
