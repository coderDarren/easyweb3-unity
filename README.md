# easyweb3-unity

### PREFACE

This package was primed for the Unity Asset Store, but Unity mentioned to me that they want nothing to do with Web3 from their creators; and have since removed similar projects from their store.

For that reason I have no choice but to put this repo out into the wild, otherwise it will just collect dust.

This project is a comprehensive blockchain reader for any EVM chain. Due to massive security implications, there are no wallet management, key management, or transaction sender functions in here as it would likely compromise the players.

### UNITY SUPPORT

This package is compatible with Unity 2019.4.38f1 and higher.

### PACKAGE SUPPORT

I am here to look at pull requests and engage the community to make this a worthwhile project. If you have questions or feature requests please join the Discord https://discord.com/invite/3NV829ch76.

### Intro
By using this asset you will easily be able to access data across three major blockchains; Ethereum, Binance Smart Chain, and Polygon. Everything will work out of the box as we provide test nodes for you to use. With Easy! Web3 you can continuously scan the chain to parse out transactions you care about, make complex custom contract calls, build ERC20 and ERC721 objects, quickly download player NFTs, and extend the base Contract class to support your own contracts.

### Free Nodes For Mainnets and Testnets
You will be granted free access to use the nodes provided in this package. You can replace the node URLs by using an overloaded Contract constructor, explained in the API section of the PDF doc. You will be provided, out of the box, with the following blockchain access:
Ethereum Mainnet
Ethereum Ropsten Testnet
Binance Smart Chain Mainnet
Binance Smart Chain Testnet
Polygon Mainnet
Polygon Testnet

### Feature-Rich Demo Scene
The demo scene will help you explore the possibilities of what you can do with this package. By studying it, you will understand how to test your blockchain functions, make custom smart contract calls, interact with standards like ERC-20 and ERC-721 (NFT), check user balances, enforce asset-based zones in-game, and scan the blockchain for any data you might need.

### UniswapV2 Scanner (Example Scanner)
To encourage creativity around data scanning we provided an example scanner for UniswapV2 transactions. This continuously outputs all UniswapV2 swaps, liquidity adds, and liquidity removals to a textbox. Any contract or wallet address can be scanned, not just Uniswap!

### Custom Encoder & Decoder
The encoder and decoder power this asset to provide one-liner custom contract calls. You effectively write pseudo-Solidity code to pull chain data. Look at all code examples in the PDF doc.
```
List<object> _outputs = await _contract.CallFunction("isBot(address)", new string[]{"bool"}, new string[]{"0x000...000"});
bool _isBot = (bool)_outputs[0];
```

### Trigger-Based NFT Loader
By studying the TriggerNFTLoader.cs file from the demo you will learn how to access NFTs for your users.

### Trigger-Based Balance Action
The TriggerBalanceAction.cs script is nice if you want to restrict areas of your game to users that hold a certain balance of a particular cryptocurrency.

# API

## ChainId
A simple enumeration of supported chains.
```
ChainId.ETH_MAINNET
ChainId.ETH_ROPSTEN
ChainId.BSC_MAINNET
ChainId.BSC_TESTNET
ChainId.MATIC_MAINNET
ChainId.MATIC_TESTNET
```

## Web3ify
Provides access to Nethereum’s web3 object for accessing the blockchain. Manages the node url.

```
property ChainId chainId
```

#### FUNCTIONS
```
async Task<BigInteger> GetChainId()
async Task<Transaction> GetTransaction(_hash)
async Task<TransactionReceipt> GetTransactionReceipt(_hash))
async Task<HexBigInteger> GetBlockNumber()
async Task<BlockWithTransactions> GetTransactionsOnBlock(_blockNum)
async Task<List<Transaction>> ScanAll(_onScanComplete)
```

## Web3Utils
Provides an interface for conversions to and from hex and strings, addresses, and integers.

#### FUNCTIONS
```
static string FunctionHash(string)
static string HexToString(string)
static string HexAddressToString(string)
static string AddressToHexString(string)
static string StringToHexBigInteger(string)
static string StringToHexString(string)
```

## Contract
Extends Web3ify. Provides custom contract call and blockchain scanning functionality for a given address. This class can be extended for any custom contract.
```
property BigInteger TotalSupply
property BigInteger Decimals
property string Name
property string Owner
property string Symbol
```

#### FUNCTIONS
```
double ValueFromDecimals(_value)
async Task<List<object>> CallFunction(string, string[], string[])
async Task<List<Transaction>> Scan(_onScanComplete)
```

## ERC20
Extends Contract, which extends Web3ify. Provides a basic interface for calling ERC20 functions.

#### FUNCTIONS
```
async Task<bool> Load()
async Task<BigInteger> GetTotalSupply()
async Task<BigInteger> GetDecimals()
async Task<string> GetName()
async Task<string> GetSymbol()
async Task<string> GetOwner()
async Task<BigInteger> GetBalanceOf(_addr)
async Task<BigInteger> GetAllowance(_owner, _spender)
```

## ERC721
Extends Contract, which extends Web3ify. Provides a basic interface for calling ERC721 functions.

#### FUNCTIONS
```
async Task<bool> Load()
async Task<BigInteger> GetTotalSupply()
async Task<string> GetName()
async Task<string> GetSymbol()
async Task<BigInteger> GetBalanceOf(_addr)
async Task<BigInteger> GetTokenOfOwnerByIndex(_owner, _index)
async Task<string> GetToken(_tokenId)
async Task<string> GetOwnerOf(_tokenId)
async Task<string> GetApproved(_tokenId)
async Task<bool> IsApprovedForAll(_owner, _operator)
async Task<List<NFT>> GetOwnerNFTs(_owner, _onProgress, _onFail)
```

## NFT
Provides a structure for NFT metadata to output to
```
property int Id
property string Uri
property NFTData Data
```

## NFTData
```
property string title
property string image
property string name
property string description
property NFTAttribute[] attributes
```

## NFTAttribute
```
property string trait_type
property string value
```

## Transaction
Wraps around the Nethereum Transaction object. Provides more usability for decoding hex input data from blockchain transactions.
```
property string MethodId
property Transaction Data
```

#### FUNCTIONS
```
List<object> GetInputs(string[])
```

## Wallet
Provides a quick interface for grabbing native and ERC-20 balances for a given address.

```
property string ShortAddress
```

#### FUNCTIONS
```
async Task<double> GetNativeBalance()
async Task<double> GetERC20Balance(_contract)
async Task<BigInteger> GetTransactionCount()
```
