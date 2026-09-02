<h1 align="center">UnipolyChain — Node</h1>

<p align="center">
  <b>An EVM-compatible, Nethermind-based public blockchain.</b><br/>
  Anyone can run a node and join the network — <b>no approval, no whitelist, no permission required.</b>
</p>

<p align="center">
  <a href="https://rpc.unpchain.com">RPC</a> ·
  <a href="https://explorer.unpchain.com">Explorer</a> ·
  <a href="https://block.unpchain.com">Validator dashboard</a> ·
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
- [Validators](#validators)
- [Independently verify the chain (genesis & validators)](#independently-verify-the-chain-genesis--validators)
- [Ports & firewall](#ports--firewall)
- [Build from source](#build-from-source)
  - [Build & run on Ubuntu without Docker](#build--run-on-ubuntu-without-docker-compile-from-source-run-the-binary-in-a-shell)
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

| Purpose             | URL                             |
|---------------------|---------------------------------|
| JSON‑RPC (HTTP)     | `https://rpc.unpchain.com`      |
| Block explorer      | `https://explorer.unpchain.com` |
| Validator dashboard | `https://block.unpchain.com`    |

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

### Confirm the network is open (no whitelist)

The bootnode's devp2p port `30304` (TCP **and** UDP) is reachable from **any IP on the public
internet** — there is no allowlist, no approval, and no whitelisting step. You can verify this
yourself before you build anything:

```bash
# TCP reachability to the public bootnode (expect: open / succeeded)
nc -vz 167.86.100.150 30304
```

Prefer an independent, off‑your‑machine check? Use any hosted TCP prober, e.g.
`https://check-host.net/check-tcp?host=167.86.100.150:30304` — probes from multiple countries
all report the port **open**. Once your node is up it will also discover further peers on its
own via discovery.

---

## Add UnipolyChain to a wallet (MetaMask)

Add a custom network with:

- **Network name:** UnipolyChain
- **New RPC URL:** `https://rpc.unpchain.com`
- **Chain ID:** `47382916`
- **Currency symbol:** `UNP`
- **Block explorer URL:** `https://explorer.unpchain.com`

---

## Validators

UnipolyChain is secured by a set of independent **Clique (Proof‑of‑Authority) validators**.
Each validator runs its own Nethermind node and takes turns sealing blocks. A validator's
public, on‑chain identity is its **signer address** — the address that signs the blocks it
produces.

> **The RPC endpoint for the whole network is `https://rpc.unpchain.com`.**
> Every validator secures this one chain, so this single public RPC is how you reach — and
> independently verify — all of them. The authoritative validator list is stored **in
> consensus** and can be read at any moment with `clique_getSigners` (see
> [verification](#independently-verify-the-chain-genesis--validators)); the table below must
> always match that on‑chain result.

> 📊 **Live validator dashboard: [`https://block.unpchain.com`](https://block.unpchain.com)**
> A public, real‑time view of the validator set: who is producing blocks, whose turn is next in
> the round‑robin, per‑validator block share, and a 24‑hour history of every block (proposer,
> whether it carried transactions, and in‑turn/out‑of‑turn seal). Every figure is recomputed from
> the public RPC and `clique_getSigners`/`clique_getBlockSigner`, so it can be independently
> reproduced — no trust required.

| #  | Validator          | Signer address (on‑chain identity)             | RPC endpoint                | Status     |
|----|--------------------|------------------------------------------------|-----------------------------|------------|
| 0  | Main Signer        | `0xf600b7e0f98ecda33d3fc1348af1f0172ef27ceb`   | `https://rpc.unpchain.com`  | ✅ active  |
| 1  | Validator 01       | `0x65f28ff7608b24f441efb830ddcc9007c3662ad0`   | `https://rpc.unpchain.com`  | ✅ active  |
| 2  | Validator 02       | `0x74627432c23e4e62757a21b30ffe5a2e231df851`   | `https://rpc.unpchain.com`  | ✅ active  |
| 3  | Validator 03       | `0x29db2863d506d1ab18a2f113e6d6855bdec6eb23`   | `https://rpc.unpchain.com`  | ✅ active  |
| 4  | Validator 04       | `0x8abe7701cde31ce03f1daf40524858983846bd72`   | `https://rpc.unpchain.com`  | ✅ active  |
| 5  | Validator 05       | `0x9b3829e9579fb66f02762c9e27b254cded1df13d`   | `https://rpc.unpchain.com`  | ✅ active  |
| 6  | Validator 06       | `0x39960c8fc6a6157d806deb6110b90e4657a58fbc`   | `https://rpc.unpchain.com`  | ✅ active  |
| 7  | Validator 07       | `0x04088960128d85a5c32ebfa58c3eefe65237ba9d`   | `https://rpc.unpchain.com`  | ✅ active  |
| 8  | Validator 08       | `0xc34a6f695b6e824d1009d7587b7ba4ea730db79e`   | `https://rpc.unpchain.com`  | ✅ active  |
| 9  | Validator 10       | `0x9517e10cef403af64ff33603f41fe952d698f3fb`   | `https://rpc.unpchain.com`  | ✅ active  |

Confirm this exact set yourself — it is read straight from consensus, so it cannot be faked:

```bash
curl -s -X POST https://rpc.unpchain.com -H 'Content-Type: application/json' \
  --data '{"jsonrpc":"2.0","method":"clique_getSigners","params":[],"id":1}'
```

**Notes**

- The validator set is **live‑verifiable at all times**; if it changes, `clique_getSigners`
  reflects it immediately. Always treat the on‑chain result as the source of truth.
- The authority set is being expanded across independent operators; new validators join through
  the standard Clique governance call (`clique_propose`) by the existing authorities. Running a
  node is separate and open to everyone today — see
  [Run a node in one command](#run-a-node-in-one-command).
- For security (DoS protection of block‑producing nodes), each validator's **own** JSON‑RPC port
  is kept private/firewalled. Public interaction and verification go through the shared network
  RPC `https://rpc.unpchain.com`; anyone who wants a local endpoint can simply
  [run their own node](#run-a-node-in-one-command) and use `http://localhost:8545`.

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

> **Which client / which tag do I build?** Build **this repository** — do **not** download a
> stock [NethermindEth](https://github.com/NethermindEth/nethermind) release binary. UnipolyChain
> runs a client that is derived from the Nethermind `1.32.0` codebase but carries this chain's
> genesis, `networkId` (`47382916`) and Clique parameters compiled in; a stock upstream binary of
> **any** version (`1.39.x` included) does **not** carry them and will fail the P2P handshake
> (`0` peers). The version to pin is **UnipolyChain's own release tag in this repo**, not a
> NethermindEth tag.
>
> **Pinned release:** [`v1.0.0`](https://github.com/Mohammadali-Ghods/UnipolyChainNode/releases/tag/v1.0.0)
> (based on Nethermind `1.32.0`). `main` always points at the latest good release, so building
> `main` is equally fine; pin the tag if you want a reproducible, immutable reference.

The one‑command flow above already builds from source. To build the image manually from the
pinned release:

```bash
git clone https://github.com/Mohammadali-Ghods/UnipolyChainNode.git
cd UnipolyChainNode
git checkout v1.0.0          # UnipolyChain's pinned client (or stay on `main` for latest)
docker build -t unpchainnode:latest .
```

This produces the `nethermind` node binary with UnipolyChain's chain configuration
(chainId `47382916`, UNPChain genesis) baked in. The client source lives under
[`src/Nethermind`](src/Nethermind); the chain spec is
[`src/Nethermind/Nethermind.Runner/chainspec/foundation.json`](src/Nethermind/Nethermind.Runner/chainspec/foundation.json).

### Build & run on Ubuntu without Docker (compile from source, run the binary in a shell)

**Docker is optional.** The node is a standard **.NET 9** application; the Dockerfile just wraps
`dotnet publish`. If your environment only allows *building from source and running a binary in a
shell*, do exactly this — no Docker, no container runtime:

**1. Install the .NET 9 SDK** (the build toolchain — it also ships the runtime that runs the node):

```bash
# Microsoft's official install script — no root / package manager required:
curl -sSL https://dot.net/v1/dotnet-install.sh -o dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 9.0            # installs to ~/.dotnet
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$PATH:$HOME/.dotnet"
dotnet --version                              # expect 9.0.x
```

> Where it is packaged you can instead use `sudo apt-get update && sudo apt-get install -y dotnet-sdk-9.0`.

**2. Get the pinned source:**

```bash
git clone https://github.com/Mohammadali-Ghods/UnipolyChainNode.git
cd UnipolyChainNode
git checkout v1.0.0            # UnipolyChain's pinned client (or stay on `main` for latest)
```

**3. Build (publish) the node** — this is the *same* command the Dockerfile runs, without Docker:

```bash
dotnet publish src/Nethermind/Nethermind.Runner \
  -c Release -o "$HOME/unpchain-node" --sc false -p:NuGetAudit=false
```

It writes a ready‑to‑run folder to `~/unpchain-node` containing the `nethermind` executable plus
the bundled `configs/`, `chainspec/foundation.json` and `Data/` (UNPChain genesis, networkId
`47382916`). `--sc false` is framework‑dependent (uses the .NET 9 runtime installed in step 1).
Want a **fully standalone** binary that needs no runtime on the run host? Use `--sc true -r linux-x64`
instead (larger output). Compiling the full solution takes several minutes the first time (it
also ReadyToRun‑precompiles the assemblies).

**4. Run the binary in your shell** (public, non‑mining node):

```bash
cd "$HOME/unpchain-node"
./nethermind --config mainnet \
  --Init.IsMining false \
  --Init.EnableUnsecuredDevWallet false \
  --Network.StaticPeers "enode://1e9863365795ea0cb16f4c524694a28e37a9b35a411965bb152a62c63735e6531af7cca65e3c8faeb6049a94535ba9516df2616411142e0d5143de001f075705@167.86.100.150:30304" \
  --JsonRpc.Enabled true --JsonRpc.Host 0.0.0.0 --JsonRpc.Port 8545 \
  --JsonRpc.EnabledModules "Eth,Net,Web3,Subscribe,TxPool,Clique,Health"
```

`./nethermind` is a native launcher; `dotnet nethermind.dll --config mainnet …` is equivalent.
`--config mainnet` loads the bundled `configs/mainnet.json`, which points at
`chainspec/foundation.json` (UNPChain genesis / networkId `47382916`). The database is written
under the working directory (`nethermind_db/…`) and persists across restarts — **no resync
needed** if you keep it. Keep it alive with `systemd`, `tmux` or `nohup`, then confirm with the
[sync checks](#verify-your-node-is-syncing) (`eth_chainId` must return `0x2d30184`).

> **Switching from a different/stock client?** If a database directory was previously written by a
> **stock NethermindEth** binary (any `1.3x` release) or any other client, point this node at a
> **fresh, empty `BaseDbPath`** rather than reusing that database — mixing databases across
> different clients can leave it in an inconsistent state. A clean full sync from block `0` is
> cheap here: UNPChain blocks are near‑empty, so a fresh node reaches the chain head in only a
> few minutes (independently reproducible — see the sync checks). After that first sync the
> database persists and no further resync is needed.

**Run as a background service (survives logout/reboot)** — a minimal `systemd` unit:

```ini
# /etc/systemd/system/unpchain-node.service
[Unit]
Description=UnipolyChain full node
After=network-online.target
Wants=network-online.target

[Service]
Type=simple
WorkingDirectory=%h/unpchain-node
Environment=DOTNET_ROOT=%h/.dotnet
ExecStart=%h/unpchain-node/nethermind --config mainnet \
  --Init.IsMining false --Init.EnableUnsecuredDevWallet false \
  --Network.StaticPeers "enode://1e9863365795ea0cb16f4c524694a28e37a9b35a411965bb152a62c63735e6531af7cca65e3c8faeb6049a94535ba9516df2616411142e0d5143de001f075705@167.86.100.150:30304" \
  --JsonRpc.Enabled true --JsonRpc.Host 0.0.0.0 --JsonRpc.Port 8545 \
  --JsonRpc.EnabledModules "Eth,Net,Web3,Subscribe,TxPool,Clique,Health"
Restart=always
RestartSec=5

[Install]
WantedBy=multi-user.target
```

```bash
systemctl daemon-reload && systemctl enable --now unpchain-node
journalctl -u unpchain-node -f      # follow the sync
```

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

**Does joining require Docker? Can I build from source and run the binary directly in a shell (e.g. plain Ubuntu, no container runtime)?**
No Docker required. Docker is only a convenience wrapper around `dotnet publish`. Install the
**.NET 9 SDK**, then:

```bash
git clone https://github.com/Mohammadali-Ghods/UnipolyChainNode.git
cd UnipolyChainNode && git checkout v1.0.0
dotnet publish src/Nethermind/Nethermind.Runner -c Release -o "$HOME/unpchain-node" --sc false -p:NuGetAudit=false
cd "$HOME/unpchain-node" && ./nethermind --config mainnet --Init.IsMining false --Init.EnableUnsecuredDevWallet false \
  --Network.StaticPeers "enode://1e9863365795ea0cb16f4c524694a28e37a9b35a411965bb152a62c63735e6531af7cca65e3c8faeb6049a94535ba9516df2616411142e0d5143de001f075705@167.86.100.150:30304"
```

Full step‑by‑step (SDK install, self‑contained option, RPC flags) is in
[Build & run on Ubuntu without Docker](#build--run-on-ubuntu-without-docker-compile-from-source-run-the-binary-in-a-shell).
Build **this** repo (tag `v1.0.0`) — never a stock NethermindEth release binary; only this repo
carries the chain spec / genesis / networkId `47382916`.

**My node is stuck at a height just below a multiple of 30000 (e.g. `2009999`) — the next block never imports (`InvalidExtraData` / "extra data too long").**
This is the single most common issue and it has a one‑line fix. Every `epoch` block (every
30000 blocks: 30000, 60000, … `2010000`, …) is a **Clique checkpoint block** whose header
`extraData` embeds the *entire* validator list (`32` vanity bytes + `20 × N` signer addresses
+ `65` seal bytes). With `N = 10` validators that checkpoint header is **297 bytes**. Older
copies of this chain spec capped `maximumExtraDataSize` at `256` (`0x100`), so the node
**rejected the valid checkpoint block** and halted at the block right before it (`2009999`).
The fix is already in this repository — `maximumExtraDataSize` is now `0x400` (1024 bytes,
room for ~46 validators). **Update and restart — you do NOT need to resync from genesis:**

```bash
cd UnipolyChainNode
git pull                       # get the fixed chain spec
cd node
docker compose up -d --build   # rebuild the node; the unpchain-data volume is preserved
docker compose logs -f         # you should see it import 2010000 and keep going
```

Your node resumes from `2009999`, re‑validates block `2010000` under the corrected rule,
accepts it, and continues. (Only delete the `unpchain-data` volume if you *want* a clean
resync — it is not required here.) You can confirm the checkpoint is valid on the live chain:

```bash
curl -s -X POST https://rpc.unpchain.com -H 'Content-Type: application/json' \
  --data '{"jsonrpc":"2.0","method":"eth_getBlockByNumber","params":["0x1EAB90",false],"id":1}'
# block 2010000 — note its 297-byte extraData (the full signer list)
```

**I downloaded a stock NethermindEth release (e.g. the `1.39.3` binary) and now it can't find peers (`0` peers, best block `0`). Which tag should I build instead?**
Do **not** use a NethermindEth upstream release binary — build **this repository**, and pin
UnipolyChain's own tag [`v1.0.0`](https://github.com/Mohammadali-Ghods/UnipolyChainNode/releases/tag/v1.0.0)
(or just build `main`). The live network runs the client in this repo (derived from Nethermind
`1.32.0`) with UnipolyChain's chain spec, genesis, and `networkId` (`47382916`) compiled in.
A generic upstream binary of any version does **not** carry this chain's configuration, so during
the devp2p handshake it presents a different genesis / networkId and **our nodes reject the
connection** — you get `0` peers and stay at block `0`. The fix:

```bash
git clone https://github.com/Mohammadali-Ghods/UnipolyChainNode.git
cd UnipolyChainNode
git checkout v1.0.0            # or stay on main
cd node
docker compose up -d --build   # wires in the correct chain spec + public bootnode
```

(If you insist on a newer upstream client, you must port this repo's chain spec —
`node/chainspec.json` and the values in [Network parameters](#network-parameters) — into it,
matching `networkId`, genesis hash and `maximumExtraDataSize: 0x400`; otherwise it will not peer.
The supported, tested path is to build this repo.)

**`net_peerCount` stays at `0`.**
Make sure you built from this repo (not a third‑party or upstream image). Confirm `eth_chainId`
returns `0x2d30184`. Check outbound access to the bootnode: `nc -vz 167.86.100.150 30304`
(it is open to the whole internet — no whitelist). If you changed client versions, see the two
entries above.

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
