# Cognito
output "user_pool_id" {
  value = aws_cognito_user_pool.usuarios.id
}
output "user_pool_client_id" {
  value = aws_cognito_user_pool_client.app_cliente.id
}

# Backend
output "image_uri" {
  value = aws_ecr_repository.app_repo.repository_url
}
output "redis_endpoint" {
  value = aws_elasticache_cluster.redis.cache_nodes[0].address
}
output "rds_endpoint" {
  value = aws_db_instance.postgres.endpoint
}

# Frontend
output "s3_bucket_name" {
  value = aws_s3_bucket.frontend.bucket
}