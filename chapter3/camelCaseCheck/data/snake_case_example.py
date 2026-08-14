"""Validation helpers using snake_case naming convention."""


def validate_email(email_address):
    if "@" not in email_address:
        return False
    local_part, _, domain = email_address.partition("@")
    if not local_part or "." not in domain:
        return False
    return True


def format_full_name(first_name, last_name):
    stripped_first = first_name.strip()
    stripped_last = last_name.strip()
    if not stripped_first or not stripped_last:
        return ""
    return f"{stripped_first} {stripped_last}"


def calculate_order_total(item_prices, tax_rate=0.0):
    subtotal = sum(item_prices)
    tax_amount = subtotal * tax_rate
    return round(subtotal + tax_amount, 2)


if __name__ == "__main__":
    print(validate_email("user@example.com"))
    print(format_full_name("  ada  ", "lovelace"))
    print(calculate_order_total([12.50, 3.75, 9.99], tax_rate=0.08))
