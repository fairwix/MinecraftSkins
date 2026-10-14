#!/bin/sh
set -eu

cert_dir=/etc/nginx/certs
cert_file="$cert_dir/server.crt"
key_file="$cert_dir/server.key"

mkdir -p "$cert_dir"
if [ ! -s "$cert_file" ] || [ ! -s "$key_file" ]; then
    umask 077
    openssl req -x509 -nodes -newkey rsa:2048 -days 30 \
        -keyout "$key_file" \
        -out "$cert_file" \
        -subj "/CN=localhost" \
        -addext "subjectAltName=DNS:localhost,IP:127.0.0.1"
fi

chmod 0600 "$key_file"
