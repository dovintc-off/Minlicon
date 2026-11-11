use tokio::runtime::Builder;
use std::thread;

struct ServerPlayer {
    pub id: u32,
    pub x: f32, // Координаты в дробных числах (float)
}

// Блок логики (методы) для игрока
impl ServerPlayer {
    // Функция-конструктор (new)
    pub fn new(id: u32) -> Self {
        Self { id, x: 0.0 }
    }

    // Метод изменения координат. Принимает &mut self, чтобы менять данные
    pub fn move_right(&mut self, speed: f32) {
        self.x += speed;
    }
}

fn get_threadings() -> usize {
    let thread_count = thread::available_parallelism().map(|n| n.get()).unwrap_or(1);
    thread_count
}

fn main() {
    let threads = get_threadings();

    let runtime = Builder::new_multi_thread()
        .worker_threads(threads)
        .enable_all()
        .build()
        .unwrap();
    
    runtime.block_on(async {println!("Доступно потоков {}", threads)});
    

    let mut player = ServerPlayer::new(1);
    
    player.move_right(12.7);

    // Координаты из f32 в i32 (каст типов через as) для сетки тайлов
    let tile_index = player.x as i32;

    println!("Игрок {} на координате: {:.1}", player.id, player.x);
    println!("Индекс тайла на сетке: {tile_index}");

    // Простое ветвление
    if tile_index > 10 {
        println!("Статус: Игрок ушел далеко от спавна!");
    }
}
