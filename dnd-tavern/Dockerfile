FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM nginx:alpine
COPY --from=build /app/dist/dnd-tavern/browser/*.txt /usr/share/nginx/html/
COPY --from=build /app/dist/dnd-tavern/browser/*.json /usr/share/nginx/html/
COPY --from=build /app/dist/dnd-tavern/browser/browser/. /usr/share/nginx/html/
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]