#!/bin/bash

# Test image URL (using web service name since we're in Docker network)
IMAGE_URL="http://web-proxy/images/img1.jpg"

# echo "First request:"
# curl -X GET -I "$IMAGE_URL"

# echo -e "\nSecond request:"
# curl -X GET -I "$IMAGE_URL"

# echo -e "\nThird request:"
# curl -X GET -I "$IMAGE_URL"

echo "First request:"
http -h GET "$IMAGE_URL"

echo -e "\nSecond request:" 
http -h GET "$IMAGE_URL"

echo -e "\nThird request:"
http -h GET "$IMAGE_URL"
