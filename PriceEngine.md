# PriceEngineReflection

A small demo that calls the private methods of a decompiled third-party `PriceEngine` class using reflection, instead of its public `CalculatePayable` method.

## What it does

`PriceEngineWrapper` resolves each private step of the pricing pipeline (`ComputeSubtotal`, `ApplyCoupon`, `ApplyVolumeDiscount`, `ApplyLoyaltyDiscount`, `ApplyVat`, `RoundMoney`) via `MethodInfo`, cached once in the constructor, then invokes them in sequence to reproduce the original calculation.

## Why

Practice exercise for:
- Using `Type.GetMethod` with `BindingFlags` to access non-public members
- Caching `MethodInfo` instead of re-resolving it on every call
- Invoking methods dynamically via `MethodInfo.Invoke`


## Structure

- `ACA.PriceEngine.dll` — third-party engine (decompiled for reference only)
- `PriceEngineWrapper` — reflection-based wrapper around the engine
- `PriceEngineReflection` — console app entry point