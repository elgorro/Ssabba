# The hugo-book theme needs Hugo >= 0.158, so build with the official image.
# That image declares VOLUME /project, so build outside it — writes to /project are discarded.
FROM ghcr.io/gohugoio/hugo:v0.165.0 AS build
USER root
WORKDIR /src
COPY . .
# baseURL matches the Traefik path prefix the site is served under.
RUN hugo --source /src --destination /src/public --minify --baseURL /docs/

FROM nginxinc/nginx-unprivileged:alpine AS final
COPY --from=build /src/public /usr/share/nginx/html/docs
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 8080
