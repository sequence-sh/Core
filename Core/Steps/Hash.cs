﻿using System.Security.Cryptography;
using System.Text;

namespace Sequence.Core.Steps;

/// <summary>
/// Produce a hash for some data using the specified hash algorithm
/// </summary>
[AllowConstantFolding]
public class Hash : CompoundStep<StringStream>
{
    /// <inheritdoc />
    protected override async ValueTask<Result<StringStream, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var data = await Data.Run(stateMonad, cancellationToken);

        if (data.IsFailure)
            return data.ConvertFailure<StringStream>();

        var algorithm = await Algorithm.Run(stateMonad, cancellationToken);

        if (algorithm.IsFailure)
            return algorithm.ConvertFailure<StringStream>();

        var hashAlgorithm = Create(algorithm.Value.Value);

        var hash = await hashAlgorithm.ComputeHashAsync(
            data.Value.GetStream().stream,
            cancellationToken
        );

        StringStream result = Convert.ToHexString(hash);

        hashAlgorithm.Dispose();

        return result;
    }

    /// <summary>
    /// The string or data stream to hash
    /// </summary>
    [Required]
    [StepProperty(1)]
    public IStep<StringStream> Data { get; set; } = null!;

    /// <summary>
    /// The hash algorithm to use
    /// </summary>
    [StepProperty(2)]
    [DefaultValueExplanation("MD5")]
    public IStep<SCLEnum<Enums.HashAlgorithm>> Algorithm { get; set; } =
        new SCLConstant<SCLEnum<Enums.HashAlgorithm>>(
            new SCLEnum<Enums.HashAlgorithm>(Enums.HashAlgorithm.MD5)
        );

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = new SimpleStepFactory<Hash, StringStream>();

    private static HashAlgorithm Create(Enums.HashAlgorithm algorithm)
    {
        return algorithm switch

        {
            Enums.HashAlgorithm.MD5 => MD5.Create(),
            Enums.HashAlgorithm.SHA1 => SHA1.Create(),
            Enums.HashAlgorithm.SHA256 => SHA256.Create(),
            Enums.HashAlgorithm.SHA384 => SHA384.Create(),
            Enums.HashAlgorithm.SHA512 => SHA512.Create(),
            _ => throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null)
        };
    }
}
