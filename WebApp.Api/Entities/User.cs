using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace WebApp.Api.Entities;

public partial class User : IdentityUser
{

    public string FullName { get; private set; } = null!;
    public string? AvatarUrl { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public string? Address { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; private set; }

    public virtual Cart? Cart { get; set; }
    public virtual ICollection<BookDonation> BookDonationDonors { get; set; } = new List<BookDonation>();

    public virtual ICollection<BookDonation> BookDonationReceivedByEmployees { get; set; } = new List<BookDonation>();

    public virtual ICollection<BookDonation> BookDonationReviewedByEmployees { get; set; } = new List<BookDonation>();

    public virtual ICollection<BookRequest> BookRequestCustomers { get; set; } = new List<BookRequest>();

    public virtual ICollection<BookRequest> BookRequestReviewedByEmployees { get; set; } = new List<BookRequest>();


    public virtual ICollection<CustomerAddress> CustomerAddresses { get; set; } = new List<CustomerAddress>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<ProductReview> ProductReviewCustomers { get; set; } = new List<ProductReview>();

    public virtual ICollection<ProductReview> ProductReviewRepliedByEmployees { get; set; } = new List<ProductReview>();

    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();

    public virtual ICollection<ReturnRequest> ReturnRequestCustomers { get; set; } = new List<ReturnRequest>();

    public virtual ICollection<ReturnRequest> ReturnRequestHandledByEmployees { get; set; } = new List<ReturnRequest>();

    public virtual ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();

    public virtual ICollection<SupportMessage> SupportMessages { get; set; } = new List<SupportMessage>();

    public virtual ICollection<SupportTicket> SupportTicketAssignedEmployees { get; set; } = new List<SupportTicket>();

    public virtual ICollection<SupportTicket> SupportTicketCustomers { get; set; } = new List<SupportTicket>();

    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();

    public static User CreateCustomer(string email, string fullName, string? phoneNumber = null)
    {
        // TODO: XÁC THỰC EMAIL CÓ TỒN TẠI HAY KHÔNG
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email không được để trống", nameof(email));
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Họ tên không được để trống", nameof(fullName));

        return new User
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            PhoneNumber = phoneNumber,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void EnsureCanLogin()
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("Tài khoản của bạn hiện đang bị khóa hoặc ngưng hoạt động.");
        }

        // Kiểm tra tài khoản có bị Lockout do đăng nhập sai nhiều lần hay không
        if (LockoutEnabled && LockoutEnd.HasValue && LockoutEnd.Value > DateTimeOffset.UtcNow)
        {
            var remainingMinutes = Math.Max(1, (int)Math.Ceiling((LockoutEnd.Value - DateTimeOffset.UtcNow).TotalMinutes));
            throw new InvalidOperationException($"Tài khoản tạm thời bị khóa do nhập sai mật khẩu nhiều lần. Vui lòng thử lại sau {remainingMinutes} phút.");
        }
    }

    public void RecordLoginSuccess()
    {
        EnsureCanLogin();
        LastLoginAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void UpdateProfile(string fullName, string? address, DateTime? dateOfBirth)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Họ tên không được để trống", nameof(fullName));

        FullName = fullName;
        Address = address;
        DateOfBirth = dateOfBirth;
    }

    public void UpdateAvatar(string? avatarUrl)
    {
        AvatarUrl = avatarUrl;
    }
}
