resource "aws_cognito_user_pool" "usuarios" {
  name                     = "user-pool"
  auto_verified_attributes = ["email"]
  
  password_policy {
    minimum_length    = 8
    require_uppercase = true
    require_lowercase = true
    require_numbers   = true
    require_symbols   = false
  }
}

resource "aws_cognito_user_pool_client" "app_cliente" {
  name         = "app-web"
  user_pool_id = aws_cognito_user_pool.usuarios.id

  explicit_auth_flows = [
    "ALLOW_USER_PASSWORD_AUTH",
    "ALLOW_REFRESH_TOKEN_AUTH",
    "ALLOW_USER_SRP_AUTH",
    "ALLOW_ADMIN_USER_PASSWORD_AUTH"
  ]

  generate_secret = false
}