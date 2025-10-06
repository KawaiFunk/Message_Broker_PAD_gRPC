package grpc.subscriber.exec;

import java.util.concurrent.*;

public final class SafeExecutor {
    private static final int CORES = Math.max(2, Runtime.getRuntime().availableProcessors());
    private static final ExecutorService POOL =
            new ThreadPoolExecutor(
                    CORES, CORES * 2,
                    60L, TimeUnit.SECONDS,
                    new ArrayBlockingQueue<>(1024),
                    r -> { Thread t = new Thread(r, "subscriber-worker"); t.setDaemon(true); return t; },
                    new ThreadPoolExecutor.CallerRunsPolicy());

    private SafeExecutor() {}
    public static void submit(Runnable task) { POOL.submit(task); }
    public static void shutdown() { POOL.shutdown(); }
}
