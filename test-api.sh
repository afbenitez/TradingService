#!/bin/bash

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

BASE_URL="https://localhost:7179"
API_URL="$BASE_URL/api/trades"

echo -e "${BLUE}?? Testing Trading Service API${NC}"
echo ""

# Test 1: Health Check
echo -e "${YELLOW}1. Testing Health Check...${NC}"
HEALTH_RESPONSE=$(curl -s -k "$BASE_URL/health")
if [ $? -eq 0 ]; then
    echo -e "${GREEN}? Health Check: $HEALTH_RESPONSE${NC}"
else
    echo -e "${RED}? Health Check failed${NC}"
fi
echo ""

# Test 2: Create a Buy Trade
echo -e "${YELLOW}2. Creating a BUY trade...${NC}"
BUY_TRADE_RESPONSE=$(curl -s -k -X POST "$API_URL" \
  -H "Content-Type: application/json" \
  -d '{
    "symbol": "AAPL",
    "quantity": 100,
    "price": 150.50,
    "tradeType": 1,
    "userId": "testuser"
  }')

if [ $? -eq 0 ]; then
    echo -e "${GREEN}? BUY Trade Created:${NC}"
    echo "$BUY_TRADE_RESPONSE" | jq '.' 2>/dev/null || echo "$BUY_TRADE_RESPONSE"
else
    echo -e "${RED}? BUY Trade creation failed${NC}"
fi
echo ""

# Test 3: Create a Sell Trade
echo -e "${YELLOW}3. Creating a SELL trade...${NC}"
SELL_TRADE_RESPONSE=$(curl -s -k -X POST "$API_URL" \
  -H "Content-Type: application/json" \
  -d '{
    "symbol": "GOOGL",
    "quantity": 50,
    "price": 2800.75,
    "tradeType": 2,
    "userId": "testuser"
  }')

if [ $? -eq 0 ]; then
    echo -e "${GREEN}? SELL Trade Created:${NC}"
    echo "$SELL_TRADE_RESPONSE" | jq '.' 2>/dev/null || echo "$SELL_TRADE_RESPONSE"
else
    echo -e "${RED}? SELL Trade creation failed${NC}"
fi
echo ""

# Test 4: Get All Trades
echo -e "${YELLOW}4. Getting all trades...${NC}"
ALL_TRADES_RESPONSE=$(curl -s -k "$API_URL")
if [ $? -eq 0 ]; then
    echo -e "${GREEN}? All Trades Retrieved:${NC}"
    echo "$ALL_TRADES_RESPONSE" | jq '.' 2>/dev/null || echo "$ALL_TRADES_RESPONSE"
else
    echo -e "${RED}? Failed to get trades${NC}"
fi
echo ""

# Test 5: Get Trades for specific user
echo -e "${YELLOW}5. Getting trades for testuser...${NC}"
USER_TRADES_RESPONSE=$(curl -s -k "$API_URL?userId=testuser")
if [ $? -eq 0 ]; then
    echo -e "${GREEN}? User Trades Retrieved:${NC}"
    echo "$USER_TRADES_RESPONSE" | jq '.' 2>/dev/null || echo "$USER_TRADES_RESPONSE"
else
    echo -e "${RED}? Failed to get user trades${NC}"
fi
echo ""

# Test 6: Get Trade Statistics
echo -e "${YELLOW}6. Getting trade statistics for testuser...${NC}"
STATS_RESPONSE=$(curl -s -k "$API_URL/statistics/testuser")
if [ $? -eq 0 ]; then
    echo -e "${GREEN}? Trade Statistics Retrieved:${NC}"
    echo "$STATS_RESPONSE" | jq '.' 2>/dev/null || echo "$STATS_RESPONSE"
else
    echo -e "${RED}? Failed to get trade statistics${NC}"
fi
echo ""

# Test 7: Invalid Trade (should fail)
echo -e "${YELLOW}7. Testing invalid trade (should fail)...${NC}"
INVALID_TRADE_RESPONSE=$(curl -s -k -X POST "$API_URL" \
  -H "Content-Type: application/json" \
  -d '{
    "symbol": "",
    "quantity": -100,
    "price": 0,
    "tradeType": 1,
    "userId": ""
  }')

if [[ "$INVALID_TRADE_RESPONSE" == *"error"* ]] || [[ "$INVALID_TRADE_RESPONSE" == *"validation"* ]]; then
    echo -e "${GREEN}? Invalid Trade Properly Rejected:${NC}"
    echo "$INVALID_TRADE_RESPONSE" | jq '.' 2>/dev/null || echo "$INVALID_TRADE_RESPONSE"
else
    echo -e "${RED}? Invalid trade was not properly rejected${NC}"
    echo "$INVALID_TRADE_RESPONSE"
fi
echo ""

echo -e "${BLUE}?? API Testing Completed!${NC}"
echo ""
echo -e "${YELLOW}?? Manual Testing URLs:${NC}"
echo -e "?? Swagger UI: ${GREEN}$BASE_URL${NC}"
echo -e "?? Health Check: ${GREEN}$BASE_URL/health${NC}"
echo -e "?? All Trades: ${GREEN}$API_URL${NC}"
echo -e "?? User Statistics: ${GREEN}$API_URL/statistics/testuser${NC}"