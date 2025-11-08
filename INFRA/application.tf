# ECR Repository
resource "aws_ecr_repository" "app_repo" {
  name = "espectaculos-web"

  image_scanning_configuration {
    scan_on_push = true
  }

  tags = {
    Name = "espectaculos-web"
  }
}

# ECS Cluster
resource "aws_ecs_cluster" "app_cluster" {
  name = "dotnet-cluster"

  tags = {
    Name = "dotnet-cluster"
  }
}

# 12. Application Load Balancer
resource "aws_lb" "app_alb" {
  name               = "myapp-alb"
  internal           = false
  load_balancer_type = "application"
  security_groups    = [aws_security_group.alb_sg.id]
  subnets            = [aws_subnet.public_a.id, aws_subnet.public_b.id]

  tags = {
    Name = "myapp-alb"
  }
}

# Target group
resource "aws_lb_target_group" "app_tg" {
  name        = "myapp-tg"
  port        = 80
  protocol    = "HTTP"
  vpc_id      = aws_vpc.main.id
  target_type = "ip" # necesario para Fargate

  health_check {
    path = "/"
    interval = 30
  }

  tags = {
    Name = "myapp-tg"
  }
}

# Listener HTTP 80
resource "aws_lb_listener" "http_listener" {
  load_balancer_arn = aws_lb.app_alb.arn
  port              = 80
  protocol          = "HTTP"

  default_action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.app_tg.arn
  }
}

# 13. ECS Task Definition
resource "aws_ecs_task_definition" "app_task" {
  family                   = "myapp-task"
  network_mode              = "awsvpc"
  requires_compatibilities  = ["FARGATE"]
  cpu                       = "256"
  memory                    = "512"
  execution_role_arn        = aws_iam_role.ecs_task_execution_role.arn
  container_definitions = jsonencode([
    {
      name      = "myapp-container"
      image     = "${aws_ecr_repository.app_repo.repository_url}:latest"
      essential = true
      portMappings = [
        {
          containerPort = 80
          hostPort      = 80
        }
      ]
      environment = [
        {
          name  = "REDIS_HOST"
          value = aws_elasticache_cluster.redis.cache_nodes[0].address
        },
        {
          name  = "DB_HOST"
          value = aws_db_instance.postgres.address
        },
        {
          name  = "DB_USER"
          value = "admin"
        },
        {
          name  = "DB_PASSWORD"
          value = "admin"
        }
      ]
    }
  ])

  runtime_platform {
    operating_system_family = "LINUX"
    cpu_architecture        = "X86_64"
  }

  tags = {
    Name = "myapp-task"
  }
}

# 15. ECS Service
resource "aws_ecs_service" "app_service" {
  name            = "myapp-service"
  cluster         = aws_ecs_cluster.app_cluster.id
  task_definition = aws_ecs_task_definition.app_task.arn
  launch_type     = "FARGATE"
  desired_count   = 1

  network_configuration {
    subnets          = [aws_subnet.public_a.id, aws_subnet.public_b.id]
    security_groups  = [aws_security_group.ecs_sg.id]
    assign_public_ip = true
  }

  load_balancer {
    target_group_arn = aws_lb_target_group.app_tg.arn
    container_name   = "myapp-container"
    container_port   = 80
  }

  depends_on = [aws_lb_listener.http_listener]

  tags = {
    Name = "myapp-service"
  }
}