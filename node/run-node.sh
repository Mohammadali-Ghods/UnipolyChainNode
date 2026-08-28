#!/usr/bin/env bash
# UnipolyChain (UNPChain) — one-command public full node launcher (plain `docker run`).
#
# Usage:
#   ./run-node.sh                 # build from source (this repo) and run
#   EXTERNAL_IP=1.2.3.4 ./run-node.sh   # also advertise your public IP for inbound peers
#
# No approval / whitelist needed. This starts a NON-mining node that syncs the live chain.
set -euo pipefail

BOOTNODE="enode://1e9863365795ea0cb16f4c524694a28e37a9b35a411965bb152a62c63735e6531af7cca65e3c8faeb6049a94535ba9516df2616411142e0d5143de001f075705@167.86.100.150:30304"
IMAGE="unpchainnode:latest"
NAME="unpchain-node"
REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"

# Build the node image from source if it isn't present yet (few minutes, one time).
if ! docker image inspect "$IMAGE" >/dev/null 2>&1; then
  echo ">> Building $IMAGE from source (this repo). First build takes a few minutes..."
  docker build -f "$REPO_ROOT/Dockerfile" -t "$IMAGE" "$REPO_ROOT"
fi

EXT_ARGS=()
if [ -n "${EXTERNAL_IP:-}" ]; then
  EXT_ARGS=(--Network.ExternalIp "$EXTERNAL_IP")
fi

docker rm -f "$NAME" >/dev/null 2>&1 || true
docker run -d --name "$NAME" --restart unless-stopped \
  -p 8545:8545 -p 30303:30303/tcp -p 30303:30303/udp \
  -v unpchain-data:/nethermind/nethermind_db \
  "$IMAGE" \
  --config mainnet \
  --Init.IsMining false \
  --Init.EnableUnsecuredDevWallet false \
  --Network.StaticPeers "$BOOTNODE" \
  --Network.MaxActivePeers 50 \
  "${EXT_ARGS[@]}" \
  --JsonRpc.Enabled true --JsonRpc.Host 0.0.0.0 --JsonRpc.Port 8545 \
  --JsonRpc.EnabledModules Eth,Net,Web3,Subscribe,TxPool,Clique,Health

echo ">> Node '$NAME' started. Follow sync:   docker logs -f $NAME"
echo ">> Check height:  curl -s -X POST http://localhost:8545 -H 'Content-Type: application/json' \\"
echo "                  --data '{\"jsonrpc\":\"2.0\",\"method\":\"eth_blockNumber\",\"params\":[],\"id\":1}'"
