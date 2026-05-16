CREATE TABLE roles (
  id INT AUTO_INCREMENT PRIMARY KEY,
  role_name VARCHAR(255) NOT NULL,
  created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
  updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  deleted_at DATETIME NULL
);

CREATE TABLE user_status (
  id INT AUTO_INCREMENT PRIMARY KEY,
  status_name VARCHAR(255) NOT NULL,
  created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
  updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  deleted_at DATETIME NULL
);

CREATE TABLE users (
  id INT AUTO_INCREMENT PRIMARY KEY,
  name VARCHAR(255) NOT NULL,
  gender_id INT NULL,
  birth_date DATE NULL,
  email VARCHAR(255) NULL,
  password_hash VARCHAR(255) NULL,
  user_status_id INT NOT NULL,
  role_id INT NOT NULL,
  created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
  updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  deleted_at DATETIME NULL,
  FOREIGN KEY (user_status_id) REFERENCES user_status(id),
  FOREIGN KEY (role_id) REFERENCES roles(id)
);

CREATE UNIQUE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_lookup ON users(role_id, user_status_id, deleted_at);

CREATE TABLE classes (
  id INT AUTO_INCREMENT PRIMARY KEY,
  teacher_id INT NOT NULL,
  name VARCHAR(255) NOT NULL,
  membership_code VARCHAR(255) NOT NULL,
  created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
  updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  deleted_at DATETIME NULL,
  FOREIGN KEY (teacher_id) REFERENCES users(id)
);

CREATE UNIQUE INDEX idx_classes_code ON classes(membership_code);
CREATE INDEX idx_classes_teacher ON classes(teacher_id, deleted_at);

CREATE TABLE class_enrollments (
  id INT AUTO_INCREMENT PRIMARY KEY,
  class_id INT NOT NULL,
  student_id INT NOT NULL,
  created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
  updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  deleted_at DATETIME NULL,
  FOREIGN KEY (class_id) REFERENCES classes(id),
  FOREIGN KEY (student_id) REFERENCES users(id)
);

CREATE INDEX idx_enrollments_composite ON class_enrollments(class_id, student_id, deleted_at);

CREATE TABLE challenges (
  id INT AUTO_INCREMENT PRIMARY KEY,
  teacher_id INT NOT NULL,
  class_id INT NULL,
  title VARCHAR(255) NOT NULL,
  description TEXT NULL,
  access_link_code VARCHAR(255) NOT NULL,
  created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
  updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  deleted_at DATETIME NULL,
  FOREIGN KEY (teacher_id) REFERENCES users(id),
  FOREIGN KEY (class_id) REFERENCES classes(id)
);

CREATE UNIQUE INDEX idx_challenges_link ON challenges(access_link_code);
CREATE INDEX idx_challenges_lookup ON challenges(teacher_id, class_id, deleted_at);

CREATE TABLE scenarios (
  id INT AUTO_INCREMENT PRIMARY KEY,
  student_id INT NOT NULL,
  challenge_id INT NULL,
  family_name VARCHAR(255) NOT NULL,
  initial_balance DECIMAL(10, 2) NOT NULL,
  created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
  updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  deleted_at DATETIME NULL,
  FOREIGN KEY (student_id) REFERENCES users(id),
  FOREIGN KEY (challenge_id) REFERENCES challenges(id)
);

CREATE INDEX idx_scenarios_lookup ON scenarios(student_id, challenge_id, deleted_at);

CREATE TABLE entries (
  id INT AUTO_INCREMENT PRIMARY KEY,
  scenario_id INT NOT NULL,
  entry_type ENUM('Income', 'Expense') NOT NULL,
  category VARCHAR(255) NOT NULL,
  amount DECIMAL(10, 2) NOT NULL,
  entry_month INT NOT NULL,
  recurrence ENUM('Monthly', 'Yearly', 'Once') NOT NULL,
  created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
  updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  deleted_at DATETIME NULL,
  FOREIGN KEY (scenario_id) REFERENCES scenarios(id)
);

CREATE INDEX idx_entries_scenario ON entries(scenario_id, deleted_at);

CREATE TABLE objectives (
  id INT AUTO_INCREMENT PRIMARY KEY,
  scenario_id INT NOT NULL,
  description VARCHAR(255) NOT NULL,
  target_value DECIMAL(10, 2) NOT NULL,
  term_months INT NOT NULL,
  created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
  updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  deleted_at DATETIME NULL,
  FOREIGN KEY (scenario_id) REFERENCES scenarios(id)
);

CREATE INDEX idx_objectives_scenario ON objectives(scenario_id, deleted_at);