FROM nginx:latest
COPY ./nginx/nginx-cache.conf /etc/nginx/nginx.conf
RUN mkdir -p /var/www/cache

CMD ["nginx", "-g", "daemon off;"]
