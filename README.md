<h1 align="center">UnipolyChain — Node</h1>

<p align="center">
  <b>An EVM-compatible, Nethermind-based public blockchain.</b><br/>
  Anyone can run a node and join the network — <b>no approval, no whitelist, no permission required.</b>
</p>

<p align="center">
  <a href="https://rpc.unpchain.com">RPC</a> ·
  <a href="https://explorer.unpchain.com">Explorer</a> ·
  <b>Chain&nbsp;ID 47382916</b>
</p>

---

## Table of contents

- [What is UnipolyChain?](#what-is-unipolychain)
- [Network parameters](#network-parameters)
- [Run a node in one command](#run-a-node-in-one-command)
- [Verify your node is syncing](#verify-your-node-is-syncing)
- [Public endpoints](#public-endpoints)
- [Bootnodes](#bootnodes)
- [Add UnipolyChain to a wallet (MetaMask)](#add-unipolychain-to-a-wallet-metamask)
- [Independently verify the chain (genesis & validators)](#independently-verify-the-chain-genesis--validators)
- [Ports & firewall](#ports--firewall)
- [Build from source](#build-from-source)
- [Advanced: use the config file directly](#advanced-use-the-config-file-directly)
- [Decentralization status & roadmap](#decentralization-status--roadmap)
- [Troubleshooting / FAQ](#troubleshooting--faq)
- [Repository layout](#repository-layout)

---

## What is UnipolyChain?

UnipolyChain (ticker **UNP**, network name **UNPChain**) is a public, EVM-compatible Layer‑1
built on [Nethermind](https://github.com/NethermindEth/nethermind). Consensus is
**Clique (Proof‑of‑Authority)** with a 15‑second block time. The network is **open**: the
protocol does not gate who may run a node, and the software, chain configuration, and
bootnodes needed to join are all published here.

> **Permissionless by design.** Running a full node does **not** require any sign‑up,
> API key, invitation, or team approval. Follow the steps below and you are on the network.

---

## Network parameters

| Parameter            | Value                                                                 |
|----------------------|-----------------------------------------------------------------------|
| Network name         | UnipolyChain (UNPChain)                                                |
| Chain ID / Network ID| **47382916**  (`0x2D30184`)                                           |
| Native currency      | **UNP** — 18 decimals                                                  |
| Consensus            | Clique PoA — `period = 15s`, `epoch = 30000`                          |
| Public RPC (HTTPS)   | `https://rpc.unpchain.com`                                             |
| Block explorer       | `https://explorer.unpchain.com`                                       |
| Genesis block hash   | `0x01e8472c2bafaf846e04f682714f24eec647d2b8d61097569a4cdfe3ea87abd8`   |
| Genesis state root   | `0xca7c5a115b7b6f61915aea709b8e35d814af76eb99e2c07d0ac55651b24a1a21`   |
| P2P / discovery port | `30303` (TCP + UDP)                                                    |
| Client               | Nethermind (this repository)                                          |

The full genesis/chain specification is in [`node/chainspec.json`](node/chainspec.json).

---

## Run a node in one command

> **Requirements:** a Linux server (2 vCPU / 4 GB RAM / 100 GB SSD is comfortable) with
> [Docker](https://docs.docker.com/engine/install/) and the Docker Compose plugin installed.

```bash
git clone https://github.com/Mohammadali-Ghods/UnipolyChainNode.git
cd UnipolyChainNode/node
docker compose up -d --build
```

That's it. The first `--build` compiles **the exact same node software the network runs**
directly from this repository, so your node's chain‑id, genesis hash, and networkId match
the live chain and it will sync automatically. Building takes a few minutes the first time;
subsequent starts are instant.

Follow the sync:

```bash
docker compose logs -f          # live logs
```

Prefer a plain `docker run`? Use the bundled launcher instead of Compose:

```bash
cd UnipolyChainNode/node
./run-node.sh                      # build + run
EXTERNAL_IP=<your.public.ip> ./run-node.sh   # also advertise yourself for inbound peers
```

> **Why build from source?** The node software pins the chain's exact `networkId` (47382916).
> A generic/older image compiled for a different id will be rejected by the P2P handshake and
> will never find peers. Building from this repo guarantees a compatible node.

---

## Verify your node is syncing

Ask your node for its height and compare it against the public RPC:

```bash
# your node
curl -s -X POST http://localhost:8545 -H 'Content-Type: application/json' \
  --data '{"jsonrpc":"2.0","method":"eth_blockNumber","params":[],"id":1}'

# the live network (reference height)
curl -s -X POST https://rpc.unpchain.com -H 'Content-Type: application/json' \
  --data '{"jsonrpc":"2.0","method":"eth_blockNumber","params":[],"id":1}'
```

Useful checks:

```bash
# chain id — must be 0x2d30184 (47382916)
curl -s -X POST http://localhost:8545 -H 'Content-Type: application/json' \
  --data '{"jsonrpc":"2.0","method":"eth_chainId","params":[],"id":1}'

# peer count — should be > 0 within a minute or two
curl -s -X POST http://localhost:8545 -H 'Content-Type: application/json' \
  --data '{"jsonrpc":"2.0","method":"net_peerCount","params":[],"id":1}'

# sync status — `false` once fully caught up
curl -s -X POST http://localhost:8545 -H 'Content-Type: application/json' \
  --data '{"jsonrpc":"2.0","method":"eth_syncing","params":[],"id":1}'
```

Your node is healthy once `net_peerCount` is non‑zero and `eth_blockNumber` is climbing
toward the reference height. The initial full sync of the whole history takes a while
depending on your bandwidth and disk.

---

## Public endpoints

| Purpose        | URL                             |
|----------------|---------------------------------|
| JSON‑RPC (HTTP)| `https://rpc.unpchain.com`      |
| Block explorer | `https://explorer.unpchain.com` |

These are provided for convenience. Once your own node is synced you can (and should) use
your local `http://localhost:8545` instead of the public endpoint.

---

## Bootnodes

Your node bootstraps by connecting to the public bootnode below (also in
[`node/bootnodes.txt`](node/bootnodes.txt) and [`node/Data/static-nodes.json`](node/Data/static-nodes.json)).
It is open to everyone:

```
enode://1e9863365795ea0cb16f4c524694a28e37a9b35a411965bb152a62c63735e6531af7cca65e3c8faeb6049a94535ba9516df2616411142e0d5143de001f075705@167.86.100.150:30304
```

The provided Compose file and launcher already wire this in via `--Network.StaticPeers`, so
you don't need to configure anything by hand.

---

## Add UnipolyChain to a wallet (MetaMask)

Add a custom network with:

- **Network name:** UnipolyChain
- **New RPC URL:** `https://rpc.unpchain.com`
- **Chain ID:** `47382916`
- **Currency symbol:** `UNP`
- **Block explorer URL:** `https://explorer.unpchain.com`

---

## Independently verify the chain (genesis & validators)

You do not have to trust us — verify against the live RPC yourself.

**Genesis hash** (must equal the value in [Network parameters](#network-parameters)):

```bash
curl -s -X POST https://rpc.unpchain.com -H 'Content-Type: application/json' \
  --data '{"jsonrpc":"2.0","method":"eth_getBlockByNumber","params":["0x0",false],"id":1}'
```

**Current validator set** (Clique authorities) — verifiable on‑chain at any time:

```bash
curl -s -X POST https://rpc.unpchain.com -H 'Content-Type: application/json' \
  --data '{"jsonrpc":"2.0","method":"clique_getSigners","params":[],"id":1}'

# or a snapshot at a specific block:
curl -s -X POST https://rpc.unpchain.com -H 'Content-Type: application/json' \
  --data '{"jsonrpc":"2.0","method":"clique_getSnapshot","params":[],"id":1}'
```

`clique_getSigners` returns the authoritative list of block‑sealing validators, so anyone —
including exchanges — can confirm exactly who secures the chain, directly from consensus data.

---

## Ports & firewall

| Port         | Proto    | Purpose                    | Expose to internet?                              |
|--------------|----------|----------------------------|--------------------------------------------------|
| `30303`      | TCP+UDP  | devp2p peering + discovery | **Yes**, if you want other nodes to dial you     |
| `8545`       | TCP      | JSON‑RPC                   | **No** by default — restrict to trusted IPs      |

To be reachable for **inbound** peers, open `30303/tcp` and `30303/udp` and start the node
with your public IP:

```bash
EXTERNAL_IP=<your.public.ip> ./run-node.sh
# or uncomment the --Network.ExternalIp lines in node/docker-compose.yml
```

A node can still sync perfectly well **outbound‑only** (behind NAT, no open ports) — it will
just not accept incoming connections.

---

## Build from source

The one‑command flow above already builds from source. To build the image manually:

```bash
git clone https://github.com/Mohammadali-Ghods/UnipolyChainNode.git
cd UnipolyChainNode
docker build -t unpchainnode:latest .
```

This produces the `nethermind` node binary with UnipolyChain's chain configuration
(chainId `47382916`, UNPChain genesis) baked in. The client source lives under
[`src/Nethermind`](src/Nethermind); the chain spec is
[`src/Nethermind/Nethermind.Runner/chainspec/foundation.json`](src/Nethermind/Nethermind.Runner/chainspec/foundation.json).

---

## Advanced: use the config file directly

If you build/run Nethermind yourself (outside the provided image), a ready‑made public‑node
config is in [`node/config/unpchain-node.cfg`](node/config/unpchain-node.cfg). It disables
mining, uses the shipped chain spec, and points at the bootnode. Paths inside it are relative
to the `node/` directory:

```bash
cd node
/path/to/nethermind --config config/unpchain-node.cfg
```

---

## Decentralization status & roadmap

- **Consensus:** Clique PoA. The current validator set is published on‑chain and can be read
  by anyone via `clique_getSigners` (see [verification](#independently-verify-the-chain-genesis--validators)).
- **Node participation is already permissionless** — this repository is everything needed to
  join as a full node without asking anyone.
- **Validator set:** the project is expanding the authority set across independent,
  geographically distributed operators. New validators are added through the standard Clique
  governance call (`clique_propose`) by the existing authorities. Adding a validator is a
  governance action and is separate from running a node — running a node is open to all today.

---

## Troubleshooting / FAQ

**`net_peerCount` stays at `0`.**
Make sure you built from this repo (not a third‑party image). Confirm `eth_chainId` returns
`0x2d30184`. Check outbound access to the bootnode: `nc -vz 167.86.100.150 30304`.

**`eth_chainId` returns something other than `0x2d30184`.**
You are running an incompatible client/spec. Rebuild from this repository.

**Sync is slow.**
Full historical sync scales with disk and bandwidth. Use an SSD/NVMe disk and keep the
container running; progress is persisted in the `unpchain-data` Docker volume.

**Where is my data?**
In the Docker named volume `unpchain-data` (mounted at `/nethermind/nethermind_db`). Remove
it only if you want to resync from genesis.

**Can I run an RPC provider for others?**
Yes. Enable the RPC modules you need and put a reverse proxy / rate limiter in front of
`8545`. Do not expose `Admin`/`Personal` modules publicly.

---

## Repository layout

```
.
├── Dockerfile                # builds the UnipolyChain node image (from src/Nethermind)
├── README.md                 # this file
├── node/                     # everything you need to run a public node
│   ├── docker-compose.yml    # one-command node (build + run)
│   ├── run-node.sh           # plain `docker run` launcher
│   ├── bootnodes.txt         # public bootnode enode(s)
│   ├── chainspec.json        # UnipolyChain genesis / chain specification
│   ├── config/
│   │   └── unpchain-node.cfg # Nethermind config for a public full node
│   └── Data/
│       └── static-nodes.json # bootnode(s) in Nethermind static-nodes format
└── src/Nethermind/           # the Nethermind client source for this chain
```

---

<p align="center"><sub>UnipolyChain is open. If you can read this repo, you can run a node.</sub></p>
