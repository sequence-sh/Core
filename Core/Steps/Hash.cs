using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps;

/// <summary>
/// Produce a hash for some data using the specified hash algorithm
/// </summary>
public class Hash : CompoundStep<StringStream>
{
    /// <inheritdoc />
    protected override async Task<Result<StringStream, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var data = await Data.Run(stateMonad, cancellationToken);

        if (data.IsFailure)
            return data.ConvertFailure<StringStream>();

        var algorithm = await Algorithm.Run(stateMonad, cancellationToken);

        if (algorithm.IsFailure)
            return algorithm.ConvertFailure<StringStream>();

        var hashAlgorithm = Create(algorithm.Value);

        var hash = await hashAlgorithm.ComputeHashAsync(
            data.Value.GetStream().stream,
            cancellationToken
        );

        StringStream result = Encoding.UTF8.GetString(hash);

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
    public IStep<Enums.HashAlgorithm> Algorithm { get; set; } =
        new EnumConstant<Enums.HashAlgorithm>(Enums.HashAlgorithm.MD5);

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
