#!/bin/bash

mongosh 127.0.0.1/appDb --username "auctionhouse-user" --password "Test-1234" /scripts/remove_unasigned_auction_images.js
